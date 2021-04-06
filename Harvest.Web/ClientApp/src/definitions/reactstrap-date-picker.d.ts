declare module 'reactstrap-date-picker' {
  import * as React from 'react';

  import DatePicker from './DatePicker'

  export default DatePicker;
}

declare module 'DatePicker' {
  import * as React from 'react';

  export type DatePickerClearButtonElement = string | Object;

  export type DatePickerPreviousButtonElement = string | Object;

  export type DatePickerNextButtonElement = string | Object;

  export type DatePickerCalendarPlacement = string | ((...args: any[]) => any);

  export type DatePickerChildren = React.ReactNode[] | React.ReactNode;

  export interface DatePickerProps {
    defaultValue?: string;
    value?: string;
    required?: boolean;
    className?: string;
    style?: Object;
    minDate?: string;
    maxDate?: string;
    cellPadding?: string;
    autoComplete?: string;
    placeholder?: string;
    dayLabels?: any[];
    monthLabels?: any[];
    onChange?: (...args: any[]) => any;
    onClear?: (...args: any[]) => any;
    onBlur?: (...args: any[]) => any;
    onFocus?: (...args: any[]) => any;
    autoFocus?: boolean;
    disabled?: boolean;
    weekStartsOn?: number;
    clearButtonElement?: DatePickerClearButtonElement;
    showClearButton?: boolean;
    previousButtonElement?: DatePickerPreviousButtonElement;
    nextButtonElement?: DatePickerNextButtonElement;
    calendarPlacement?: DatePickerCalendarPlacement;
    dateFormat?: string;
    size?: string;
    calendarContainer?: Object;
    id?: string;
    name?: string;
    showTodayButton?: boolean;
    todayButtonLabel?: string;
    customControl?: Object;
    roundedCorners?: boolean;
    showWeeks?: boolean;
    children?: DatePickerChildren;
    onInvalid?: (...args: any[]) => any;
    noValidate?: boolean;
    valid?: boolean;
    invalid?: boolean;
    customInputGroup?: Object;
    inputRef?: any;
  }

  export default class DatePicker extends React.Component<DatePickerProps, any> {
    render(): JSX.Element;

  }

}

declare module 'DatePickerCalendar' {
  import * as React from 'react';

  export interface DatePickerCalendarProps {
    selectedDate?: Object;
    displayDate: Object;
    minDate?: string;
    maxDate?: string;
    onChange: (...args: any[]) => any;
    dayLabels: any[];
    cellPadding: string;
    weekStartsOn?: number;
    showTodayButton?: boolean;
    todayButtonLabel?: string;
    roundedCorners?: boolean;
    showWeeks?: boolean;
  }

  export default class DatePickerCalendar extends React.Component<DatePickerCalendarProps, any> {
    render(): JSX.Element;

  }

}

declare module 'DatePickerHeader' {
  import * as React from 'react';

  export type DatePickerHeaderPreviousButtonElement = string | Object;

  export type DatePickerHeaderNextButtonElement = string | Object;

  export interface DatePickerHeaderProps {
    displayDate: Object;
    minDate?: string;
    maxDate?: string;
    onChange: (...args: any[]) => any;
    monthLabels: any[];
    previousButtonElement: DatePickerHeaderPreviousButtonElement;
    nextButtonElement: DatePickerHeaderNextButtonElement;
  }

  export default class DatePickerHeader extends React.Component<DatePickerHeaderProps, any> {
    render(): JSX.Element;

  }

}

