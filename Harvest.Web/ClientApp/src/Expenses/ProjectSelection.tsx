import React, { useEffect, useState } from "react";

import { FormGroup, Input } from "reactstrap";

import { Project } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";
import { useParams } from "react-router-dom";

interface RouteParams {
  team?: string;
}

interface Props {
  selectedProject: (projectId: number) => void;
}

export const ProjectSelection = (props: Props) => {
  const [projects, setProjects] = useState<Project[]>();

  const { team } = useParams<RouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get list of projects
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/Project/Active?team=${team}`
      );

      if (response.ok) {
        const projects = await response.json();

        getIsMounted() && setProjects(projects);
      }
    };

    cb();
  }, [getIsMounted]);

  if (projects === undefined) {
    return <div>Loading...</div>;
  } else if (projects.length === 0) {
    return <div>No active projects found.</div>;
  }

  return (
    <div className="card-wrapper">
      <div className="card-content">
        <h2>Choose a project to view it's expenses</h2>
        <FormGroup>
          <Input
            type="select"
            name="select"
            onChange={(e) => props.selectedProject(parseInt(e.target.value))}
          >
            <option value={0}>Select a Project</option>
            {projects.map((p) => (
              <option key={`project-${p.id}`} value={p.id}>
                {p.name} ({p.principalInvestigator.name})
              </option>
            ))}
          </Input>
        </FormGroup>
      </div>
    </div>
  );
};
