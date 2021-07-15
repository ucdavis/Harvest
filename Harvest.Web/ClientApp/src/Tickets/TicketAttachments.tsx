import { useMemo, useState } from "react";
import { TicketAttachment, TicketDetails, BlobFile } from "../types";
import { FormGroup, Label } from "reactstrap";
import { FileUpload } from "../Shared/FileUpload";

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

  const [ticketLoc, setTicketLoc] = useState<TicketDetails>({
  } as TicketDetails);

    const updateFiles = async (attachments: BlobFile[]) => {
        const response = await fetch(`/Ticket/UploadFiles/${projectId}/${props.ticket.id}/`, {
          method: "POST",
          headers: {
              Accept: "application/json",
              "Content-Type": "application/json",
          },
          body: JSON.stringify({ Attachments: attachments }),
        });

        if (response.ok) {
            const data = await response.json() as TicketAttachment[];

            setTicket({ ...ticket, attachments: [...ticket.attachments, ...data] });

            setTicketLoc((ticket) => ({ ...ticket, newAttachments: [] }));
            alert("Attachement(s) saved.");
        } else {
            alert("Something went wrong, please try again");
        }
    };



  return (
    <div>
      <h2>Ticket Attachments</h2>
      {ticketAttachments === undefined || ticketAttachments.length === 0 ? (
        <p> No Messages Yet!!!</p>
      ) : null}

      {ticketAttachments.map((attachment) => (
          <p key={`attachment-${attachment.id}`}>
              {attachment.fileName} from {attachment.createdBy.name}
        </p>
      ))}
      <FormGroup>
        <Label>Attach files?</Label>
        <FileUpload
                  files={ticketLoc.newAttachments || []}
          setFiles={(f) => {
              setTicketLoc((ticket) => ({ ...ticket, newAttachments: [...f] }));
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

              return { ...ticket, newAttachments: [...ticket.newAttachments] };
            })
          }
        ></FileUpload>
      </FormGroup>
    </div>
  );
};
