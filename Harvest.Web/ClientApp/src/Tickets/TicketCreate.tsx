import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Project, Ticket } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import DatePicker from "react-date-picker";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { FileUpload } from "../Shared/FileUpload";
import { ShowFor } from "../Shared/ShowFor";
import { usePromiseNotification } from "../Util/Notifications";

interface RouteParams {
  projectId?: string;
}

export const TicketCreate = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [ticket, setTicket] = useState<Ticket>({
    requirements: "",
    name: "",
  } as Ticket);
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();
        setProject(proj);
        setTicket((t) => ({ ...t, projectId: proj.id }));
      }
    };

    cb();
  }, [projectId]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }
  const create = async () => {
    // TODO: validation

    const request = fetch(`/Ticket/Create?projectId=${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(ticket),
    });
    setNotification(request, "Creating Ticket", "Ticket Created");

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      history.push(`/Project/Details/${data.id}`);
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="row justify-content-center">
          <div className="col-md-6 card-wrapper no-green mt-4 mb-4">
            <div className="card-content">
              <h2>Create new ticket for your project</h2>

              <FormGroup>
                <Label>Subject</Label>
                <Input
                  type="text"
                  name="name"
                  id="name"
                  value={ticket.name}
                  onChange={(e) =>
                    setTicket({ ...ticket, name: e.target.value })
                  }
                  placeholder="Enter a short description for this request"
                />
              </FormGroup>

              <FormGroup>
                <Label>What are the details of your ticket request?</Label>
                <Input
                  type="textarea"
                  name="text"
                  id="requirements"
                  value={ticket.requirements}
                  onChange={(e) =>
                    setTicket({ ...ticket, requirements: e.target.value })
                  }
                  placeholder="Enter a full description of your requirements"
                />
              </FormGroup>
              <div className="row">
                <div className="col-md-6">
                  <div className="form-group">
                    <Label>Due Date?</Label>
                    <div className="input-group" style={{ zIndex: 9000 }}>
                      <DatePicker
                        format="MM/dd/yyyy"
                        required={false}
                        clearIcon={null}
                        value={ticket.dueDate}
                        onChange={(date) =>
                          setTicket({ ...ticket, dueDate: date as Date })
                        }
                      />
                    </div>
                  </div>
                </div>
              </div>
              <FormGroup>
                <Label>Want to attach any files?</Label>
                <FileUpload
                  files={ticket.attachments || []}
                  setFiles={(f) =>
                    setTicket((tick) => ({ ...tick, attachments: [...f] }))
                  }
                  updateFile={(f) =>
                    setTicket((tick) => {
                      // update just one specific file from ticket p
                      tick.attachments[
                        tick.attachments.findIndex(
                          (file) => file.identifier === f.identifier
                        )
                      ] = { ...f };

                      return { ...tick, attachments: [...tick.attachments] };
                    })
                  }
                ></FileUpload>
              </FormGroup>
              <div className="row justify-content-center">
                <ShowFor roles={["FieldManager", "Supervisor", "PI"]}>
                  <Button className="btn-lg" color="primary" onClick={create} disabled={notification.pending}>
                    Create New Ticket
                  </Button>
                </ShowFor>
              </div>
              <div>DEBUG: {JSON.stringify(ticket)}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
