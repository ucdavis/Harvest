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
        <div className="ticket-response-card">
          <div className="row justify-content-between">
            <Col>
              <p className="lede">{ticketMessage.createdBy?.name}</p>
            </Col>
            <Col>
              <p className="ticket-timestamp text-right">
                12:30 PST 11/31/2021
              </p>
            </Col>
          </div>
          <p>{ticketMessage.message}</p>
          {/* <p key={`message-${ticketMessage.id}`}>
            <span className="ticket-responder"></span> <br />
            <span className="ticket-responder-timestamp">
              12:30 PM 11/23/2021
            </span>
            <br />
            <br />
          </p> */}
        </div>
      ))}
    </div>
  );
};
