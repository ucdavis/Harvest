import React, { useEffect, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSpinner } from "@fortawesome/free-solid-svg-icons";
import { BlobServiceClient } from "@azure/storage-blob";
import { BlobFile } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  disabled?: boolean;
  files: BlobFile[];
  setFiles: (files: BlobFile[]) => void;
  updateFile: (file: BlobFile) => void;
}

const UploadFile = async (
  sasUrl: string,
  file: BlobFile,
  dataPromise: Promise<ArrayBuffer>
) => {
  const blobServiceClient = new BlobServiceClient(sasUrl);

  // don't need to pass a container since it's in the SAS url
  const containerClient = blobServiceClient.getContainerClient("");
  const blockClient = containerClient.getBlockBlobClient(file.identifier);

  const data = await dataPromise;
  const uploadBlobResponse = await blockClient.uploadData(data, {
    blobHTTPHeaders: {
      blobContentType: file.contentType,
      blobContentDisposition: `filename=${file.fileName}`,
    },
  });

  return uploadBlobResponse;
};

export const FileUpload = (props: Props) => {
  // TODO: pass uploaded files up to parent.  perhaps allow add/remove
  const [sasUrl, setSasUrl] = useState<string>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // grab the sas token right away
    const cb = async () => {
      const response = await authenticatedFetch(`/api/File/GetUploadDetails`);

      if (response.ok) {
        const url = await response.text();
        getIsMounted() && setSasUrl(url);
      }
    };

    cb();
  }, [getIsMounted]);

  const filesChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    const addedFiles = event.target.files;

    // shouldn't be possible, but we can't upload any files if we don't have the SAS info yet
    if (sasUrl === undefined || addedFiles === null) return;

    const newFiles: BlobFile[] = [];

    for (let i = 0; i < addedFiles.length; i++) {
      const addedFile = addedFiles[i];

      const fileId = uuidv4();

      const newFile: BlobFile = {
        identifier: fileId,
        fileName: addedFile.name,
        fileSize: addedFile.size,
        contentType: addedFile.type,
        uploaded: false,
      };

      newFiles.push(newFile);

      UploadFile(sasUrl, newFile, addedFile.arrayBuffer()).then((_) => {
        // TODO, check for an error
        // file is done uploading, so update uploaded details
        props.updateFile({ ...newFile, uploaded: true });
      });
    }

    props.setFiles([...props.files, ...newFiles]);
  };

  if (sasUrl === undefined) {
    return <></>;
  }

  return (
    <div>
      <input
        disabled={props.disabled === true}
        type="file"
        multiple={true}
        className="form-control-file"
        onChange={filesChanged}
      />
      {props.files.length > 0 && (
        <small id="emailHelp" className="form-text text-muted">
          <ul>
            {props.files.map((file, i) => (
              <li key={`file-${i}`}>
                {file.fileName}{" "}
                {file.uploaded === false ? (
                  <FontAwesomeIcon icon={faSpinner} className="fa-spin" />
                ) : null}
              </li>
            ))}
          </ul>
        </small>
      )}
    </div>
  );
};
