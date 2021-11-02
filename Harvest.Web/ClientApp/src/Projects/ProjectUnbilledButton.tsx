import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { formatCurrency } from "../Util/NumberFormatting";
import { useIsMounted } from "../Shared/UseIsMounted";

interface Props {
  projectId: number;
  remaining: number;
}

export const ProjectUnbilledButton = (props: Props) => {
  const [total, setTotal] = useState<number>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(
        `/api/expense/getunbilledtotal/${props.projectId}`
      );

      if (response.ok) {
        getIsMounted() && setTotal(Number.parseFloat(await response.text()));
      }
    };

    cb();
  }, [props.projectId, getIsMounted]);

  if (total === 0) {
    return (
      <button className="btn btn-lg btn-outline-disabled" disabled>
        Unbilled Expenses - $0.00
      </button>
    );
  }
  if (total === undefined || (total !== 0 && total <= props.remaining)) {
    return (
      <Link
        to={`/expense/unbilled/${props.projectId}`}
        className="btn btn-lg btn-outline"
      >
        View Unbilled Expenses - $
        {total === undefined ? "xx.xx" : formatCurrency(total)}
      </Link>
    );
  }

  return (
    <Link
      to={`/expense/unbilled/${props.projectId}`}
      className="btn btn-lg  btn-outline-danger"
    >
      View Unbilled Expenses - $
      {total === undefined ? "xx.xx" : formatCurrency(total)}
    </Link>
  );
};
