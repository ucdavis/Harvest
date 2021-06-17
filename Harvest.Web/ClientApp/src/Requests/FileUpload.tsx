import React, { useEffect, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import { BlobServiceClient } from "@azure/storage-blob";

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

export const FileUpload = () => {
  // TODO: pass uploaded files up to parent.  perhaps allow add/remove
  const [files, setFiles] = useState<File[]>([]);
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

    const newFiles: File[] = [];

    for (let i = 0; i < addedFiles.length; i++) {
      const addedFile = addedFiles[i];

      const fileId = uuidv4();
      // TODO: make file name unique w/ GUID or similar
      newFiles.push({
        id: fileId,
        name: addedFile.name,
        size: addedFile.size,
        type: addedFile.type,
        uploaded: false,
      });

      console.log("uploading with ", sasUrl);

      UploadFile(sasUrl, fileId, addedFile.arrayBuffer()).then((_) => {
        // TODO, check for an error

        // file is done uploading, so update uploaded details
        setFiles((f) => {
          const fileIndex = f.findIndex((file) => file.id === fileId);
          f[fileIndex].uploaded = true;

          return [...f];
        });
      });
    }

    setFiles((f) => [...f, ...newFiles]);
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
      {files.length > 0 && (
        <small id="emailHelp" className="form-text text-muted">
          <ul>
            {files.map((file) => (
              <li key={file.name}>
                {file.name} {!file.uploaded && "[spinner]"}
              </li>
            ))}
          </ul>
        </small>
      )}
    </div>
  );
};

interface File {
  id: string;
  name: string;
  size: number;
  type: string;
  uploaded: boolean;
}
