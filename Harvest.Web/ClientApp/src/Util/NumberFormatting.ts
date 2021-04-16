export const formatCurrency = (num: number) => {
  if (isNaN(num)) {
    return "??";
  }
  return num.toFixed(2).replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
};
