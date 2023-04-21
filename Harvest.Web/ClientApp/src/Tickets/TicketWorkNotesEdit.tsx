import { TicketDetails, CommonRouteParams } from "../types";
import { Button, FormGroup, Input } from "reactstrap";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useParams } from "react-router-dom";

interface Props {
  ticket: TicketDetails;
  projectId: string;
  setNotes: (notes: string) => void;
}

export const TicketWorkNotesEdit = (props: Props) => {
  const { ticket, setNotes } = props;
  const { team } = useParams<CommonRouteParams>();

  const [notification, setNotification] = usePromiseNotification();

  const update = async () => {
    // TODO: validation

    const request = authenticatedFetch(
      `/api/${team}/Ticket/UpdateWorkNotes?projectId=${props.projectId}&ticketId=${ticket.id}`,
      {
        method: "POST",
        body: JSON.stringify(ticket.workNotes),
      }
    );

    setNotification(request, "Saving Work Notes", "Work Notes Saved");

    await request;
  };

  return (
    <>
      <h3>Work Notes</h3>
      <FormGroup className="ticket-reply-form">
        <Input
          type="textarea"
          name="text"
          id="workNotes"
          value={ticket.workNotes ? ticket.workNotes : ""}
          onChange={(e) => setNotes(e.target.value)}
        />
      </FormGroup>
      <div className="row">
        <div className="col text-right">
          <Button
            className="btn btn-sm"
            color="primary"
            onClick={update}
            disabled={notification.pending}
          >
            Update Work Notes
          </Button>
        </div>
      </div>
    </>
  );
};
