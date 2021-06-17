import React, { useState } from "react";

export const FileUpload = () => {
  // TODO: pass uploaded files up to parent.  perhaps allow add/remove
  const [files, setFiles] = useState<File[]>([]);

  const filesChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
    const addedFiles = event.target.files;

    if (addedFiles == null) {
      return;
    }

    console.log(addedFiles);

    const newFiles: File[] = [];

    for (let i = 0; i < addedFiles.length; i++) {
      const addedFile = addedFiles[i];

      // TODO: make file name unique w/ GUID or similar
      newFiles.push({
        name: addedFile.name,
        size: addedFile.size,
        type: addedFile.type,
        uploaded: false,
      });
    }

    setFiles((f) => [...f, ...newFiles]);
  };

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
              <li key={file.name}>{file.name} {!file.uploaded && '[spinner]'}</li>
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
