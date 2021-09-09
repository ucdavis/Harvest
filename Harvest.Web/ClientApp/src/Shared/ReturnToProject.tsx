import { Link } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";

interface Props {
  projectId?: string;
}

export const ReturnToProject = (props: Props) => (
  <div className="card-green-bg green-bg-border">
    <div className="card-content">
      <Link
        to={`/project/details/${props.projectId}`}
        className="btn btn-sm btn-link"
      >
        Back to project details <FontAwesomeIcon icon={faArrowLeft} />
      </Link>
    </div>
  </div>
);
