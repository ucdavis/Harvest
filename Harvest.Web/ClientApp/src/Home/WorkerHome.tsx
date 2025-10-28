import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Project, CommonRouteParams } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";
import { MobileAppQRCodeModal } from "../Shared/QR";

export const WorkerHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [showMobileAppQR, setShowMobileAppQR] = useState(false);

  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const getProjectsWithRecentExpenses = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/expense/GetRecentExpensedProjects`
      );
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjectsWithRecentExpenses();
  }, [getIsMounted, team]);

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
        <li className="list-group-item">
          <Link to={`/${team}/expense/entry`}>
            Enter Expenses for Any Project
          </Link>
        </li>
        <li className="list-group-item">
          <Link to={`/${team}/mobile/token`}>Link Mobile App</Link>
        </li>
        <li className="list-group-item">
          <Link to="#" onClick={() => setShowMobileAppQR(true)}>
            Download Mobile App (iPhone)
          </Link>
        </li>
        {projects.map((project) => (
          <li key={project.id} className="list-group-item">
            <Link to={`/${team}/expense/entry/${project.id}`}>
              Enter Expenses for {project.name}{" "}
              <span className="badge badge-light">Recent</span>
            </Link>
          </li>
        ))}
      </ul>

      {/* Mobile App QR Code Modal */}
      {showMobileAppQR && (
        <MobileAppQRCodeModal onClose={() => setShowMobileAppQR(false)} />
      )}
    </>
  );
};
