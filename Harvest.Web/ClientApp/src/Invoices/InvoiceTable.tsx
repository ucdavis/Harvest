import { useMemo } from "react";
import { Link } from "react-router-dom";
import { Cell, Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Invoice } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  invoices: Invoice[];
  team: string;
  compact?: boolean;
}

export const InvoiceTable = (props: Props) => {
  console.log(props);
  const invoiceData = useMemo(() => props.invoices, [props.invoices]);
  const columns: Column<Invoice>[] = useMemo(
    () => [
      {
        Cell: (data: Cell<Invoice>) => (
          <div>
            <Link
              to={`/${props.team}/invoice/details/${data.row.original.projectId}/${data.row.original.id}`}
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
        id: "createdOn",
        Header: "Created",
        accessor: (row) =>
          `${row.createdOn} ${new Date(row.createdOn).toLocaleDateString()}`,
        Cell: (data: Cell<Invoice>) =>
          new Date(data.row.original.createdOn).toLocaleDateString(),
      },
      {
        Header: "Total",
        accessor: (row) => "$" + formatCurrency(row.total),
      },
    ],
    [props.team]
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "createdOn", desc: true }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={invoiceData}
      initialState={initialState}
      hideFilters={props.compact || false}
      hidePagination={props.compact}
    />
  );
};
