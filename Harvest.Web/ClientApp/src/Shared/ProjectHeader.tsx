import { Project, ProjectAccount } from "../types";

interface Props {
  project: Project;
  title: string;
}

export const ProjectHeader = (props: Props) => {
  const { project, title } = props;

  return (
    <div className="card-content">
      <div className="quote-info row">
        <div className="col-md-6">
          <h2 id="request-title">{title}</h2>
          <p className="lede">PI: {project.principalInvestigator.name}</p>
          <p>
            Created {new Date(project.createdOn).toDateString()} by{" "}
            {project.createdBy.name}
          </p>
          <p className="lede">Accounts</p>
          {project.accounts.map((acc: ProjectAccount) => (
            <p key={acc.id}>
              {acc.name} {acc.percentage}%
            </p>
          ))}
          <p className="lede">Requirements</p>
          <p>{project.requirements}</p>
        </div>
        <div className="col-md-6 quote-info-box">
          <div className="row">
            <div className="col">
              <p className="lede">Status</p>
              <p className={`project-status-${project.status}`}>
                {project.status}
              </p>
              <p className="lede">Type</p>
              <p>{project.cropType}</p>
            </div>
            <div className="col">
              <p className="lede">Timeline</p>
              <p>
                {new Date(project.start).toLocaleDateString()} through{" "}
                {new Date(project.end).toLocaleDateString()}
              </p>
              <p className="lede">Crops</p>
              <p>{project.crop}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
