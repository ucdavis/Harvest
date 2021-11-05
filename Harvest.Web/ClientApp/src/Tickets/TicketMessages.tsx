import { useMemo } from "react";
import { Col } from "reactstrap";
import { TicketMessage } from "../types";

interface Props {
  messages: TicketMessage[];
}

export const TicketMessages = (props: Props) => {
  const ticketMessages = useMemo(() => props.messages, [props.messages]);

  return (
    <div>
      <h2>Conversation</h2>
      {ticketMessages === undefined || ticketMessages.length === 0 ? (
        <p> No Messages Yet!!!</p>
      ) : null}
      {ticketMessages.map((ticketMessage) => (
        <div className="ticket-response-card" key={ticketMessage.id}>
          <div className="row mb-2 justify-content-between">
            <Col>
              <div className="lede">{ticketMessage.createdBy?.name}</div>
            </Col>
            <Col>
              <div className="ticket-timestamp text-right">
                {new Date(ticketMessage.createdOn + "Z").toLocaleDateString(
                  "en-US",
                  {
                    timeZone: "America/Los_Angeles",
                    year: "numeric",
                    month: "2-digit",
                    day: "2-digit",
                    hour: "2-digit",
                    minute: "2-digit",
                    second: "2-digit",
                  }
                )}
              </div>
            </Col>
          </div>
          <p>{ticketMessage.message}</p>
        </div>
      ))}
    </div>
  );
};
