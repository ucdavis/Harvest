import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { History, CommonRouteParams } from "../types";
import { HistoryTable } from "./HistoryTable";
import { ShowFor } from "../Shared/ShowFor";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

// If we need to work with the share ID, look into the invoices container. There are a few places else that would need to change as well.
interface Props {
  projectId?: string;
  compact: boolean;
}

export const RecentHistoriesContainer = (props: Props) => {
  const [histories, setHistories] = useState<History[]>([]);
  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/ListHistory/?projectId=${props.projectId}&maxRows=5`
      );

      if (response.ok) {
        getIsMounted() && setHistories(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted, team]);

  return (
    <ShowFor roles={["FieldManager", "Supervisor", "PI", "Finance", "Shared"]}>
      <div id="recentHistoryContainer">
        <div className="row justify-content-between">
          <div className="col">
            <h3>Recent Histories</h3>
          </div>
          <div className="col text-right">
            <Link to={`/${team}/project/history/${props.projectId}`}>
              View All
            </Link>
          </div>
        </div>

        <HistoryTable
          compact={props.compact}
          histories={histories}
        ></HistoryTable>
      </div>
    </ShowFor>
  );
};
