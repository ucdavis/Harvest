import { useMemo } from "react";
import { TicketAttachment } from "../types";

interface Props {
  attachments: TicketAttachment[];
}

export const TicketAttachments = (props: Props) => {
  const ticketAttachments = useMemo(() => props.attachments, [
    props.attachments,
  ]);

  return (
    <div>
      <h2>Ticket Attachments TODO!!!</h2>
      {ticketAttachments === undefined || ticketAttachments.length === 0 ? (
        <p> No Messages Yet!!!</p>
      ) : null}

      {ticketAttachments.map((attachment) => (
        <p key={`attachment-${attachment.id}`}>
          {attachment.fileName} from {attachment.createdBy.name}
        </p>
      ))}
    </div>
  );
};
