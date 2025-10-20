import React, { useMemo } from "react";
import { Link, useHistory } from "react-router-dom";
import { Cell, Column, TableState } from "react-table";
import { Progress } from "reactstrap";

import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import {
  DatePickerFilter,
  SelectColumnFilter,
  SelectColumnFilterRange,
  progressFilter,
  startDateFilter,
  endDateFilter,
  multiSelectFilter,
} from "../Shared/Filtering";
import { Project } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";
import { convertCamelCase } from "../Util/StringFormatting";

interface Props {
  projects: Project[];
}

export const ProjectTable = (props: Props) => {
  const history = useHistory();
  const projectData = useMemo(() => props.projects, [props.projects]);
  const columns: Column<Project>[] = useMemo(
    () => [
      {
        Cell: (data: Cell<Project>) => (
          <div>
            <p>
              <Link
                to={`/${data.row.original.team.slug}/project/details/${data.row.original.id}`}
              >
                #{data.row.original.id} {data.row.original.name}
              </Link>
            </p>
          </div>
        ),
        Header: "Name",
        maxWidth: 150,
        accessor: (row: any) => `#${row.id} ${row.name}`,
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
        accessor: (row) => (row.chargedTotal / row.quoteTotal) * 100,
        Header: "Progress",
        Filter: SelectColumnFilterRange,
        filter: "progress",
      },
      {
        id: "quote",
        Header: "Quote",
        accessor: (row) => `$${formatCurrency(row.quoteTotal)}`,
      },
      {
        id: "charged",
        Header: "Charged",
        accessor: (row) => `$${formatCurrency(row.chargedTotal)}`,
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
        Filter: SelectColumnFilter,
        filter: multiSelectFilter,
      },
      {
        id: "startDate",
        Header: "Start",
        accessor: (row) =>
          `${row.start} ${new Date(row.start).toLocaleDateString()}`,
        Cell: (data: Cell<Project>) =>
          new Date(data.row.original.start).toLocaleDateString(),
        Filter: DatePickerFilter,
        filter: "startDate",
      },
      {
        id: "endDate",
        Header: "End",
        accessor: (row) =>
          `${row.end} ${new Date(row.end).toLocaleDateString()}`,
        Cell: (data: Cell<Project>) =>
          new Date(data.row.original.end).toLocaleDateString(),
        Filter: DatePickerFilter,
        filter: "endDate",
      },
      {
        Header: "Status",
        accessor: "status",
        Cell: (data: Cell<Project>) => (
          <span className={`project-status-${data.row.original.status}`}>
            {convertCamelCase(data.row.original.status)}
          </span>
        ),
        Filter: SelectColumnFilter,
        filter: multiSelectFilter,
      },
    ],
    []
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "name" }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  const handleRowClick = (project: Project) => {
    history.push(`/${project.team.slug}/project/details/${project.id}`);
  };

  return (
    <ReactTable
      columns={columns}
      data={projectData}
      initialState={initialState}
      filterTypes={{
        progress: progressFilter,
        startDate: startDateFilter,
        endDate: endDateFilter,
      }}
      onRowClick={handleRowClick}
    />
  );
};
