import { useMemo, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import { TicketAttachment, TicketDetails, BlobFile } from "../types";
import { FormGroup, Label } from "reactstrap";
import { FileUpload } from "../Shared/FileUpload";
import {
  fetchWithFailOnNotOk,
  genericErrorMessage,
  toast,
} from "../Util/Notifications";

interface Props {
  ticket: TicketDetails;
  attachments: TicketAttachment[];
  projectId: string;
  setTicket: (ticket: TicketDetails) => void;
}

export const TicketAttachments = (props: Props) => {
  const { ticket, setTicket, projectId } = props;
  const ticketAttachments = useMemo(() => props.attachments, [
    props.attachments,
  ]);

  const [ticketLoc, setTicketLoc] = useState<TicketDetails>(
    {} as TicketDetails
  );

  const updateFiles = async (attachments: BlobFile[]) => {
    const request = fetch(
      `/Ticket/UploadFiles/${projectId}/${props.ticket.id}/`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ Attachments: attachments }),
      }
    );

    toast.promise(fetchWithFailOnNotOk(request), {
      loading: "Saving Attachment(s)",
      success: "Attachment(s) Saved",
      error: genericErrorMessage,
    });

    const response = await request;

    if (response.ok) {
      const data = (await response.json()) as TicketAttachment[];

      setTicket({ ...ticket, attachments: [...ticket.attachments, ...data] });

      setTicketLoc((ticket) => ({ ...ticket, newAttachments: [] }));
    }
  };

  return (
    <div>
      <h2>Ticket Attachments</h2>
      {ticketAttachments === undefined || ticketAttachments.length === 0 ? (
        <p> No Attachments Yet!!!</p>
      ) : null}

      <ul className="no-list-style attached-files-list">
        {ticketAttachments.map((attachment, i) => (
          <li key={`attachment-${i}`}>
            <a href={attachment.sasLink} target="_blank" rel="noreferrer">
              <FontAwesomeIcon icon={faDownload} />
              {attachment.fileName} from {attachment.createdBy.name}
            </a>
          </li>
        ))}
      </ul>
      <FormGroup>
        {!ticket.completed && (
          <>
            <Label>Attach files?</Label>
            <FileUpload
              files={ticketLoc.newAttachments || []}
              setFiles={(f) => {
                setTicketLoc((ticket) => ({
                  ...ticket,
                  newAttachments: [...f],
                }));
                updateFiles(f);
              }}
              updateFile={(f) =>
                setTicketLoc((ticket) => {
                  // update just one specific file from ticket p
                  ticket.newAttachments[
                    ticket.newAttachments.findIndex(
                      (file) => file.identifier === f.identifier
                    )
                  ] = { ...f };

                  return {
                    ...ticket,
                    newAttachments: [...ticket.newAttachments],
                  };
                })
              }
            ></FileUpload>
          </>
        )}
      </FormGroup>
    </div>
  );
};
