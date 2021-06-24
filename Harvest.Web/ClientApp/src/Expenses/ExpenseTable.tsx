import React, { useMemo } from "react";
import { Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Expense } from "../types";

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
        accessor: (row) => row.price,
      },
      {
        Header: "Total",
        accessor: (row) => row.total,
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
