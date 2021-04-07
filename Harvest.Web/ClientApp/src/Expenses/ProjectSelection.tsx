import React, { useEffect, useState } from "react";

import { FormGroup, Input } from "reactstrap";

import { Project } from "../types";

interface Props {
  selectedProject: (projectId: number) => void;
}

export const ProjectSelection = (props: Props) => {
  const [projects, setProjects] = useState<Project[]>();

  useEffect(() => {
    // get list of projects
    const cb = async () => {
      const response = await fetch(`/Project/Active`);

      if (response.ok) {
        const projects = await response.json();

        setProjects(projects);
      }
    };

    cb();
  }, []);

  if (projects === undefined) {
    return <div>Loading...</div>;
  } else if (projects.length === 0) {
    return <div>No active projects found.</div>;
  }

  return (
    <div className="card-wrapper">
      <div className="card-content">
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
