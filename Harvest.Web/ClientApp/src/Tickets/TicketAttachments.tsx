import { useMemo } from "react";
import { TicketAttachment } from "../types";

interface Props {
  attachments: TicketAttachment[];
}

export const TicketAttachments = (props: Props) => {
  const ticketAttachments = useMemo(() => props.attachments, [
    props.attachments,
  ]);

  if (ticketAttachments === undefined || ticketAttachments.length === 0) {
    return (
      <div>
        <h2>Ticket Attachments</h2>
        <p>No Attachments</p>
      </div>
    );
  }

  return (
    <div>
      <h2>Ticket Attachments TODO!!!</h2>
      <p>{ticketAttachments[0].fileName}</p>
    </div>
  );
};
