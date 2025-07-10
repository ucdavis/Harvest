import React, { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { History, Project, CommonRouteParams } from "../types";
import { HistoryTable } from "./HistoryTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { authenticatedFetch } from "../Util/Api";
import AppContext from "../Shared/AppContext";

interface RouteParams {
  projectId?: string;
}

export const HistoryListContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const { team } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>();
  const [histories, setHistories] = useState<History[]>([]);
  const userInfo = useContext(AppContext);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/ListHistory/?projectId=${projectId}`
      );

      if (response.ok) {
        getIsMounted() && setHistories(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted, team, userInfo.user.roles]);
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/Get/${projectId}`
      );

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted, team]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  if (histories.length === 0) {
    return <div>No histories found</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-content">
        <h3>Invoices</h3>
        <HistoryTable histories={histories}></HistoryTable>
      </div>
    </div>
  );
};
