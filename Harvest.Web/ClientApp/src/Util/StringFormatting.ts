export const convertCamelCase = (word: string) => {
  const newWord = word
    // Regex to insert a space before all caps
    .replace(/([A-Z])/g, " $1")
    // Regex to uppercase the first word and return the result
    .replace(/^./, (str) => {
      return str.toUpperCase();
    });

  return newWord;
};
