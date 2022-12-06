using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class CalendarHolidays : MonoBehaviour
    {
    [SerializeField] private List<Holiday> Holidays;
    private void Start()
    {
        Holidays.ForEach(holiday => holiday.CheckHoliday());
    }

}
