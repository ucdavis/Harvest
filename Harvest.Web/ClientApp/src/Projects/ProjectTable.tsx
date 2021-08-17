import React, { useMemo } from "react";
import { Link } from "react-router-dom";
import { Cell, Column, TableState } from "react-table";
import { Progress } from "reactstrap";

import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Project } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  projects: Project[];
}

export const ProjectTable = (props: Props) => {
  const projectData = useMemo(() => props.projects, [props.projects]);
  const columns: Column<Project>[] = useMemo(
    () => [
      {
        Cell: (data: Cell<Project>) => (
          <div>
            <Link to={`/project/details/${data.row.original.id}`}>
              #{data.row.original.id}
            </Link>
            <p>{data.row.original.name}</p>
          </div>
        ),
        Header: "Name",
        maxWidth: 150,
        accessor: "id",
      },
      {
        id: "pi",
        Header: "PI",
        accessor: (row) => row.principalInvestigator.name,
      },
      {
        id: "progress",
        Cell: (data: Cell<Project>) => {
          const percent =
            data.row.original.chargedTotal / data.row.original.quoteTotal;
          return <Progress style={{ width: "10em" }} value={percent * 100} />;
        },
        accessor: (row) => row.chargedTotal / row.quoteTotal,
        Header: "Progress",
      },
      {
        id: "remaining",
        Header: "Remaining",
        accessor: (row) =>
          "$" + formatCurrency(row.quoteTotal - row.chargedTotal),
      },
      {
        Header: "Crop Type",
        accessor: "cropType",
      },
      {
        id: "startDate",
        Header: "Start",
        accessor: "start",
        Cell: (data: Cell<Project>) =>
          new Date(data.value).toLocaleDateString(),
      },
      {
        id: "endDate",
        Header: "End",
        accessor: "end",
        Cell: (data: Cell<Project>) =>
          new Date(data.value).toLocaleDateString(),
      },
      {
        Header: "Status",
        accessor: "status",
        Cell: (data: Cell<Project>) => (
          <span className={`project-status-${data.row.original.status}`}>
            {data.row.original.status}
          </span>
        ),
      },
    ],
    []
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "name" }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={projectData}
      initialState={initialState}
    />
  );
};
