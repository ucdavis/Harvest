import { useParams } from "react-router-dom";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const { projectId } = useParams<RouteParams>();

  return (
    <div>
      <h3>Create quote for project: {projectId}</h3>
    </div>
  );
};
