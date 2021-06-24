import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  projectId: number;
}

export const ProjectUnbilledButton = (props: Props) => {
  const [total, setTotal] = useState<number>();

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(
        `/expense/getunbilledtotal/${props.projectId}`
      );

      if (response.ok) {
        setTotal(Number.parseFloat(await response.text()));
      }
    };

    cb();
  }, [props.projectId]);

  if (total === 0) {
    return <button className="btn btn-light">Unbilled Expenses - $0.00</button>;
  }

  return (
    <Link to={`/expense/unbilled/${props.projectId}`} className="btn btn-light">
      View Unbilled Expenses - $
      {total === undefined ? "xx.xx" : formatCurrency(total)}
    </Link>
  );
};
