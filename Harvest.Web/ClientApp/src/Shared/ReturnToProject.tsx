import { Link } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";

interface Props {
  projectId: number;
  team: string;
  shareId?: string;
}

export const ReturnToProject = (props: Props) => (
  <div className="card-green-bg green-bg-border">
    <div className="card-content">
      <div className="row justify-content-between">
        <div className="col-12 margin-left-fixer">
          <Link
            to={
              props.shareId
                ? `/${props.team}/project/details/${props.projectId}/${props.shareId}`
                : `/${props.team}/project/details/${props.projectId}`
            }
            className="btn btn-sm btn-link"
          >
            Back to project details <FontAwesomeIcon icon={faArrowLeft} />
          </Link>
        </div>
      </div>
    </div>
  </div>
);
