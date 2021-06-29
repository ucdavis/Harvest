import React, { useMemo } from "react";
import { Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Expense } from "../types";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  expenses: Expense[];
}

export const ExpenseTable = (props: Props) => {
  const expenseData = useMemo(() => props.expenses, [props.expenses]);

  const columns: Column<Expense>[] = useMemo(
    () => [
      {
        Header: "Activity",
        accessor: (row) => row.activity,
      },
      {
        Header: "Type",
        accessor: (row) => row.type,
      },
      {
        Header: "Description",
        accessor: (row) => row.description,
      },
      {
        Header: "Quantity",
        accessor: (row) => row.quantity,
      },
      {
        Header: "Rate",
        accessor: (row) => "$" + formatCurrency(row.price),
      },
      {
        Header: "Total",
        accessor: (row) => "$" + formatCurrency(row.total),
      },
      {
        Header: "Entered by",
        accessor: (row) => row.createdBy?.name,
      },
      {
        Header: "Entered on",
        accessor: (row) =>
          row.createdOn === undefined
            ? "N/A"
            : new Date(row.createdOn).toLocaleDateString(),
      },
    ],
    []
  );

  const initialState: Partial<TableState<any>> = {
    sortBy: [{ id: "Activity" }, { id: "Type" }],
    pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={expenseData}
      initialState={initialState}
    />
  );
};
