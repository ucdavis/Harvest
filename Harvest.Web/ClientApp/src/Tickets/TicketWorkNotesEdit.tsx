import { TicketDetails } from "../types";
import { Button, FormGroup, Input } from "reactstrap";
import { usePromiseNotification } from "../Util/Notifications";

interface Props {
  ticket: TicketDetails;
  projectId: string;
  setNotes: (notes: string) => void;
}

export const TicketWorkNotesEdit = (props: Props) => {
  const { ticket, setNotes } = props;

  const [notification, setNotification] = usePromiseNotification();

  const update = async () => {
    // TODO: validation

    const request = fetch(
      `/Ticket/UpdateWorkNotes?projectId=${props.projectId}&ticketId=${ticket.id}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(ticket.workNotes),
      }
    );

    setNotification(request, "Saving Work Notes", "Work Notes Saved");

    await request;
  };

  return (
    <>
      <h2>Work Notes</h2>
      <FormGroup>
        <Input
          type="textarea"
          name="text"
          id="workNotes"
          value={ticket.workNotes ? ticket.workNotes : ""}
          onChange={(e) => setNotes(e.target.value)}
        />
      </FormGroup>
      <div className="row justify-content-center">
        <Button
          className="btn"
          color="primary"
          onClick={update}
          disabled={notification.pending}
        >
          Update Work Notes
        </Button>
      </div>
    </>
  );
};
