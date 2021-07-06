import { useMemo } from "react";
import { TicketMessage } from "../types";

interface Props {
  messages: TicketMessage[];
}

export const TicketMessages = (props: Props) => {
  const ticketMessages = useMemo(() => props.messages, [
    props.messages,
  ]);

  if (ticketMessages === undefined || ticketMessages.length === 0) {
    return <div>No Messages Yet!!!</div>;
  }

  return (
    <div>
      <p>{ticketMessages[0].message}</p>
    </div>
  );
};
