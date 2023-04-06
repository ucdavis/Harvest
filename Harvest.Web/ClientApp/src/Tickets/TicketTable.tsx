import { useMemo } from "react";
import { Cell, Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Ticket } from "../types";
import { Link } from "react-router-dom";

interface Props {
  tickets: Ticket[];
  compact?: boolean;
}

export const TicketTable = (props: Props) => {
  const ticketData = useMemo(() => props.tickets, [props.tickets]);
  const columns: Column<Ticket>[] = useMemo(
    () => [
      {
        Cell: (data: Cell<Ticket>) => (
          <div>
            <Link
              to={`/${data.row.original.project.team.slug}/ticket/details/${data.row.original.projectId}/${data.row.original.id}`}
            >
              #{data.row.original.id}
            </Link>
          </div>
        ),
        Header: " ",
        maxWidth: 150,
      },
      {
        Header: "Status",
        accessor: (row) => row.status,
      },
      {
        Header: "Created",
        accessor: (row) =>
          `${row.createdOn} ${new Date(row.createdOn).toLocaleDateString()}`,
        Cell: (data: Cell<Ticket>) =>
          new Date(data.row.original.createdOn).toLocaleDateString(),
      },
      {
        Header: "Updated",
        accessor: (row) =>
          `${row.updatedOn} ${
            row.updatedOn ? new Date(row.updatedOn).toLocaleDateString() : ""
          }`,
        Cell: (data: Cell<Ticket>) =>
          data.row.original.updatedOn
            ? new Date(data.row.original.updatedOn).toLocaleDateString()
            : "",
      },
      {
        Header: "Due",
        accessor: (row) =>
          `${row.dueDate} ${
            row.dueDate ? new Date(row.dueDate).toLocaleDateString() : ""
          }`,
        Cell: (data: Cell<Ticket>) =>
          data.row.original.dueDate
            ? new Date(data.row.original.dueDate).toLocaleDateString()
            : "",
      },
      {
        Header: "Subject",
        accessor: (row) => row.name,
      },
    ],
    []
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "Updated", desc: true }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={ticketData}
      initialState={initialState}
      hideFilters={props.compact || false}
      hidePagination={props.compact}
    />
  );
};
