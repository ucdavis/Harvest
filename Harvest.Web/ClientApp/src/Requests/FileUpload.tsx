import React, { useEffect, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import { BlobServiceClient } from "@azure/storage-blob";
import { BlobFile } from "../types";

interface Props {
  files: BlobFile[];
  setFiles: (files: BlobFile[]) => void;
  updateFile: (file: BlobFile) => void;
}

const UploadFile = async (
  sasUrl: string,
  fileName: string,
  dataPromise: Promise<ArrayBuffer>
) => {
  const blobServiceClient = new BlobServiceClient(sasUrl);

  // don't need to pass a container since it's in the SAS url
  const containerClient = blobServiceClient.getContainerClient("");
  const blockClient = containerClient.getBlockBlobClient(fileName);

  const data = await dataPromise;
  const uploadBlobResponse = await blockClient.uploadData(data);

  return uploadBlobResponse;
};

export const FileUpload = (props: Props) => {
  // TODO: pass uploaded files up to parent.  perhaps allow add/remove
  const [sasUrl, setSasUrl] = useState<string>();

  useEffect(() => {
    // grab the sas token right away
    const cb = async () => {
      const response = await fetch(`/File/GetUploadDetails`);

      if (response.ok) {
        setSasUrl(await response.text());
      }
    };

    cb();
  }, []);

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

      UploadFile(sasUrl, fileId, addedFile.arrayBuffer()).then((_) => {
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
        type="file"
        multiple={true}
        className="form-control-file"
        onChange={filesChanged}
      />
      {props.files.length > 0 && (
        <small id="emailHelp" className="form-text text-muted">
          <ul>
            {props.files.map((file) => (
              <li key={file.fileName}>
                {file.fileName} {!file.uploaded && "[spinner]"}
              </li>
            ))}
          </ul>
        </small>
      )}
    </div>
  );
};