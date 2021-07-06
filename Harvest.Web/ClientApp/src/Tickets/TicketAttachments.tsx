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
    return <div>No Attachments</div>;
  }

  return (
    <div>
      <p>{ticketAttachments[0].fileName}</p>
    </div>
  );
};
