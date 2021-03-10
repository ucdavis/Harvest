import { useEffect } from "react";
import { useParams } from "react-router-dom";

interface RouteParams {
  projectId?: string;
}

export const QuoteContainer = () => {
  const { projectId } = useParams<RouteParams>();

  useEffect(() => {
    const cb = async () => {
      const response = await fetch("/Quote/3");

      if (response.ok) {
        console.log(await response.json());
      } else {
        console.log(response);
      }
    };

    cb();
  });

  return (
    <div>
      <h3>Create quote for project: {projectId}</h3>
    </div>
  );
};
