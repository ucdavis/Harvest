import { useEffect, useState, PropsWithChildren } from "react";
import { Input, Dropdown, DropdownMenu, DropdownItem, DropdownToggle, Badge } from "reactstrap";
import { useDebounce } from "../hooks/debounce";

enum CharCode {
  Enter = 13
}

interface ISearchSelectProps<T> {
  getText: (item: T) => string;
  getId: (item: T) => string | number;
  onCreate?: (name: string) => Promise<T>;
  onSelectionChanged: (selection: T[]) => void;
  onSearch: (query: string) => Promise<T[]>;
  selection: T[];
  placeholder?: string;
  multiselect?: boolean;
}

// declaring as function, since arrow functions and generic parameters don't play nicely in tsx files
export function SearchSelect<T>(props: PropsWithChildren<ISearchSelectProps<T>>) {
  const [selection, setSelection] = useState(props.selection);
  const [dropDownOpen, setDropDownOpen] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const debouncedIsOpen = useDebounce(isOpen, 1);
  const [query, setQuery] = useState("");
  const debouncedQuery = useDebounce(query, 500);
  const [searching, setSearching] = useState(false);
  const [options, setOptions] = useState<T[]>([]);

  useEffect(() => {
    const cb = async () => {
      if (debouncedQuery) {
        setSearching(true);
        setIsOpen(true);
        try {
          const items = await props.onSearch(debouncedQuery);
          // only show search results for items that are not already selected
          const selectedIds = selection.map(item => props.getId(item));
          setOptions(items.filter(item => !selectedIds.includes(props.getId(item))));
        } finally {
          setSearching(false);
        }
      } else {
        setOptions([]);
      }
    }

    cb();
  }, [debouncedQuery]);

  const selectItem = (item: T) => {
    const itemId = props.getId(item);
    if (selection.map(x => props.getId(x)).includes(itemId)) {
      return; // already selected
    }

    const newSelection = props.multiselect ? [...selection, item] : [item];
    setSelection(newSelection);
    setQuery("");
    props.onSelectionChanged(newSelection);
    setIsOpen(false);
  }

  const unSelectItem = (item: T) => {
    const itemId = props.getId(item);
    const newSelection = selection.filter(i => props.getId(i) !== itemId);
    setSelection(newSelection);
    props.onSelectionChanged(newSelection);
  }

  const handleKeypress = async (charCode: number) => {
    if (charCode === CharCode.Enter && query && props.onCreate) {
      const item = await props.onCreate(query);
      selectItem(item);
    }
  }


  return (
    <Dropdown isOpen={debouncedIsOpen} toggle={() => { /* the interface may say that toggle is optional, but nope */ }}>
      <div onBlur={() => setIsOpen(false)}>
        <DropdownToggle
          tag="span"
          data-toggle="dropdown"
          aria-expanded={dropDownOpen}>
          <Input placeholder={props.placeholder} value={query} onChange={e => setQuery(e.target.value)} onKeyPress={e => handleKeypress(e.charCode)}/>
          {selection.map((item, i) => <span key={`selectedItem_${i}`} ><Badge style={{cursor:"pointer"}} onClick={() => unSelectItem(item)}>{props.getText(item)}</Badge>{" "}</span>)}
        </DropdownToggle>
      </div>
      <DropdownMenu onBur={() => setIsOpen(false)}>
        {searching && <DropdownItem key="search" onFocus={() => setIsOpen(true)} disabled>Searching...</DropdownItem>}
        {!searching && options.map((item, i) => (<DropdownItem key={`searchResult_${i}`} onClick={() => selectItem(item)} onFocus={() => setIsOpen(true)} onBlur={() => setIsOpen(false)}>{props.getText(item)}</DropdownItem>))}
      </DropdownMenu>
    </Dropdown>
  );
};
