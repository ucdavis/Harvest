import { useEffect, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";

import { useHistory, useLocation } from "react-router-dom";
import { Team } from "../types";

// Allow selection of a team from a list of teams, then add that team slug to url and redirect
export const TeamPicker = () => {
  // get the current page location
  const location = useLocation();

  const history = useHistory();

  // store teams in state
  const [teams, setTeams] = useState<Team[]>();

  // get the teams from the server
  useEffect(() => {
    const getTeams = async () => {
      const response = await fetch("/api/Team");
      const data = await response.json();

      // if there is only one team, redirect to that team's page immediately
      if (data.length === 1) {
        history.replace(`/${data[0].slug}${location.pathname}`);
      } else {
        setTeams(data);
      }
    };
    getTeams();
  });

  // show loading message while we wait for teams
  if (!teams) {
    return <div>Loading teams...</div>;
  }

  // show the list of teams in big boxes
  return (
    <div>
      <h2>Choose Team</h2>
      <hr />
      <div className="row">
        {teams.map((team) => (
          <div className="col-md-6 card-no-underline" key={team.id}>
            <a href={`/${team.slug}${location.pathname}`}>
              <div className="card">
                <div className="card-body">
                  <h5 className="card-title">{team.name}</h5>
                  <p className="card-text">
                    FieldManagers: {team.fieldManagers}
                  </p>
                  <p className="card-text">{team.description}</p>
                  <FontAwesomeIcon icon={faCheck} /> Use this team
                </div>
              </div>
            </a>
          </div>
        ))}
      </div>
    </div>
  );
};
