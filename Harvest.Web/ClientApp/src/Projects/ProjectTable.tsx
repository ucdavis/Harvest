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
        Header: " ",
        maxWidth: 150,
      },
      {
        Header: "PI",
        accessor: (row) => row.principalInvestigator.name,
      },
      {
        Cell: (data: Cell<Project>) => {
          const percent =
            data.row.original.chargedTotal / data.row.original.quoteTotal;
          return <Progress style={{ width: "10em" }} value={percent * 100} />;
        },
        Header: "Progress",
      },
      {
        Header: "Remaining",
        accessor: (row) => '$' + formatCurrency(row.quoteTotal - row.chargedTotal)
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
