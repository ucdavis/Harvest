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
              to={`/ticket/details??projectId=${data.row.original.projectId}&id=${data.row.original.id}`}
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
          row.createdOn ? new Date(row.createdOn).toLocaleDateString() : "",
      },
      {
        Header: "Updated",
        accessor: (row) =>
          row.updatedOn ? new Date(row.updatedOn).toLocaleDateString() : "",
      },
      {
        Header: "Due",
        accessor: (row) =>
          row.dueDate ? new Date(row.dueDate).toLocaleDateString() : "",
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
    />
  );
};
