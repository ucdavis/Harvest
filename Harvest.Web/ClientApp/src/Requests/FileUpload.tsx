import React, { useEffect, useState } from "react";

import { BlobServiceClient } from "@azure/storage-blob";

const UploadFile = async (
  sasUrl: string,
  fileName: string,
  dataPromise: Promise<ArrayBuffer>
) => {
  const blobServiceClient = new BlobServiceClient(sasUrl);

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
    // shouldn't be possible, but we can't upload any files if we don't have the SAS info yet
    if (sasUrl === undefined) return;

    const addedFiles = event.target.files;

    if (addedFiles == null) {
      return;
    }

    const newFiles: File[] = [];

    for (let i = 0; i < addedFiles.length; i++) {
      const addedFile = addedFiles[i];

      // TODO: make file name unique w/ GUID or similar
      const name = addedFile.name;

      newFiles.push({
        name: name,
        size: addedFile.size,
        type: addedFile.type,
        uploaded: false,
      });

      console.log('uploading with ', sasUrl)

      UploadFile(sasUrl, name, addedFile.arrayBuffer()).then((_) => {
        // TODO, check for an error

        // file is done uploading, so update uploaded details
        setFiles((f) => {
          const fileIndex = f.findIndex((file) => file.name === name);
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
  name: string;
  size: number;
  type: string;
  uploaded: boolean;
}
