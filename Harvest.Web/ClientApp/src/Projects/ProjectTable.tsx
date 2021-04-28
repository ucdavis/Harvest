import React, { useMemo } from "react";
import { Cell, Column, TableState } from "react-table";
import { Progress } from "reactstrap";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Project } from "../types";

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
            <a href={`/project/details/${data.row.original.id}`}>
              #{data.row.original.id}
            </a>
            <p>{data.row.original.name}</p>
          </div>
        ),
        Header: " ",
        maxWidth: 150,
      },
      {
        Header: "PI",
        accessor: (row) => row.principalInvestigator.name,
      },
      {
        Cell: (data: Cell<Project>) => (
          <Progress style={{ width: "10em" }} value={Math.random() * 100} />
        ),
        Header: "Progress",
      },
      {
        Header: "Crop Type",
        accessor: (row) => row.cropType,
      },
      {
        Header: "Start",
        accessor: (row) => new Date(row.start).toLocaleDateString(),
      },
      {
        Header: "End",
        accessor: (row) => new Date(row.end).toLocaleDateString(),
      },
      {
        Header: "Status",
        accessor: (row) => row.status,
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
