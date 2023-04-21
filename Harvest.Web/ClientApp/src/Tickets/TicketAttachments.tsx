import { useMemo, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import {
  TicketAttachment,
  TicketDetails,
  BlobFile,
  CommonRouteParams,
} from "../types";
import { FormGroup, Label } from "reactstrap";
import { FileUpload } from "../Shared/FileUpload";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useParams } from "react-router-dom";

interface Props {
  ticket: TicketDetails;
  attachments: TicketAttachment[];
  projectId: string;
  setTicket: (ticket: TicketDetails) => void;
}

export const TicketAttachments = (props: Props) => {
  const { ticket, setTicket, projectId } = props;
  const { team } = useParams<CommonRouteParams>();
  const ticketAttachments = useMemo(
    () => props.attachments,
    [props.attachments]
  );

  const [ticketLoc, setTicketLoc] = useState<TicketDetails>(
    {} as TicketDetails
  );

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  const updateFiles = async (attachments: BlobFile[]) => {
    const request = authenticatedFetch(
      `/api/${team}/Ticket/UploadFiles/${projectId}/${props.ticket.id}/`,
      {
        method: "POST",
        body: JSON.stringify({ Attachments: attachments }),
      }
    );
    setNotification(request, "Saving Attachment(s)", "Attachment(s) Saved");

    const response = await request;

    if (response.ok) {
      const data = (await response.json()) as TicketAttachment[];
      if (getIsMounted()) {
        setTicket({ ...ticket, attachments: [...ticket.attachments, ...data] });

        setTicketLoc((ticket) => ({ ...ticket, newAttachments: [] }));
      }
    }
  };

  return (
    <div>
      <h2>Ticket Attachments</h2>
      {ticketAttachments === undefined || ticketAttachments.length === 0 ? (
        <p> No Attachments Yet</p>
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
              disabled={notification.pending}
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
