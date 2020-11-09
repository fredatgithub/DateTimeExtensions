﻿#region License

// 
// Copyright (c) 2011-2012, João Matos Silva <kappy@acydburne.com.pt>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

#endregion

using System;
using System.Collections.Generic;
using DateTimeExtensions.Common;
using DateTimeExtensions.WorkingDays.OccurrencesCalculators;

namespace DateTimeExtensions.WorkingDays.CultureStrategies
{
    [Locale("en-AU")]
    public class EN_AUHolidayStrategy : HolidayStrategyBase, IHolidayStrategy
    {
        public EN_AUHolidayStrategy()
        {
            // Australian public holidays vary on a state-by-state basis. These rules are based
            // holidays that are observed in at least half of Australian states and territories
            // according to https://en.wikipedia.org/wiki/Public_holidays_in_Australia

            this.InnerCalendarDays.Add(new Holiday(GlobalHolidays.NewYear));
            this.InnerCalendarDays.Add(new Holiday(AustraliaDay));
            this.InnerCalendarDays.Add(new Holiday(ChristianHolidays.GoodFriday));
            this.InnerCalendarDays.Add(new Holiday(ChristianHolidays.EasterSaturday));
            this.InnerCalendarDays.Add(new Holiday(ChristianHolidays.EasterMonday));
            this.InnerCalendarDays.Add(new Holiday(AnzacDay));
            this.InnerCalendarDays.Add(new Holiday(QueensBirthday));
            this.InnerCalendarDays.Add(new Holiday(LabourDay));
            this.InnerCalendarDays.Add(new Holiday(ChristianHolidays.Christmas));
            this.InnerCalendarDays.Add(new Holiday(GlobalHolidays.BoxingDay));
        }

        protected override IDictionary<DateTime, CalendarDay> BuildObservancesMap(int year)
        {
            IDictionary<DateTime, CalendarDay> holidayMap = new Dictionary<DateTime, CalendarDay>();
            foreach (var innerHoliday in InnerCalendarDays)
            {
                var date = innerHoliday.Day.GetInstance(year);
                if (date.HasValue)
                {
                    holidayMap[date.Value] = innerHoliday;
                }
            }

            var existingHolidayMap = new Dictionary<DateTime, CalendarDay>(holidayMap);
            foreach (var existingHoliday in existingHolidayMap)
            {
                var date = existingHoliday.Key;
                var innerHoliday = existingHoliday.Value;

                // don't move the holiday if it is easter based since it's already observated
                if (innerHoliday.Day != ChristianHolidays.Easter && innerHoliday.Day != AnzacDay)
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        AddHolidayOnNextAvailableDay(holidayMap, date, innerHoliday.Day);
                    }
                }
            }

            return holidayMap;
        }

        private void AddHolidayOnNextAvailableDay(IDictionary<DateTime, CalendarDay> calendarDayMap, DateTime date, NamedDay namedDay)
        {
            if (!calendarDayMap.ContainsKey(date) && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                calendarDayMap.Add(date, new Holiday(namedDay));
            }
            else
            {
                AddHolidayOnNextAvailableDay(calendarDayMap, date.AddDays(1), namedDay);
            }
        }

        //Last Monday in May - Spring Bank Holiday
        public static NamedDayInitializer AustraliaDay { get; } = new NamedDayInitializer(() =>
            new NamedDay("Australia Day", new FixedDayStrategy(Month.January, 26)));
        
        //1st Monday in May	- May Day Bank Holiday (not an national holiday, but observed on some regions)
        public static NamedDayInitializer MayDay { get; } = new NamedDayInitializer(() =>
            new NamedDay("May Day", new NthDayOfWeekInMonthDayStrategy(1, DayOfWeek.Monday, Month.May,
                CountDirection.FromFirst)));

        //25th April - Anzac Day
        public static NamedDayInitializer AnzacDay { get; } = new NamedDayInitializer(() =>
            new NamedDay("Anzac Day", new FixedDayStrategy(Month.April, 25)));

        //2nd Monday in June - Queen's Birthday
        public static NamedDayInitializer QueensBirthday { get; } = new NamedDayInitializer(() => 
            new NamedDay("Queen's Birthday", new NthDayOfWeekInMonthDayStrategy(2, DayOfWeek.Monday, Month.June,
                CountDirection.FromFirst)));
        
        //1nd Monday in October - Labour Day
        public static NamedDayInitializer LabourDay { get; } = new NamedDayInitializer(() =>
            new NamedDay("Labour Day", new NthDayOfWeekInMonthDayStrategy(1, DayOfWeek.Monday, Month.October,
            CountDirection.FromFirst)));
    }
}