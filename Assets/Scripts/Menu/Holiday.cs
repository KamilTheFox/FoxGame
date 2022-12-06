using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
    public class Holiday
    {
    [SerializeField] private string nameHoliday;
    public string NameHoliday => nameHoliday;
    [SerializeField] private UnityEvent Action;
    [SerializeField] private List<ObjectInitialize> Objects;
    [Serializable]
    public class ObjectInitialize
    {
        [SerializeField] private GameObject GameObject;
        [SerializeField] private TypeAction typeAction;
        private enum TypeAction
        {
            Initialize,
            Hidden,
            Reveal
        }
        public void Action()
        {
            switch (typeAction)
                { 
                case TypeAction.Initialize:
                    GameObject.Instantiate(GameObject);
                    break;
                case TypeAction.Hidden:
                    GameObject.SetActive(false);
                    break;
                case TypeAction.Reveal:
                    GameObject.SetActive(true);
                    break;
            };
            
        }

    }
    [SerializeField] private int BeginDay;
    [SerializeField] private int BeginMonth;
    [SerializeField] private int EndDay;
    [SerializeField] private int EndMonth;
    
    private enum TypeData
    {
        Day,
        Month
    }
    public bool CheckHoliday()
    {
        DateTime dateTime = DateTime.Now;
        string[] DayAndMonth = dateTime.ToString("dd,MM").Split(',');
        int[] DayAndMonthInt = new int[]
        {
            int.Parse(DayAndMonth[(int)TypeData.Day]),
            int.Parse(DayAndMonth[(int)TypeData.Month])
        };
        bool OnlyDay = DayAndMonthInt[(int)TypeData.Day] == BeginDay && DayAndMonthInt[1] == BeginMonth && EndDay == 0 && EndMonth ==  0;
        bool MultyDays = DayAndMonthInt[(int)TypeData.Day] >= BeginDay && DayAndMonthInt[(int)TypeData.Day] <= EndDay &&
                                 DayAndMonthInt[(int)TypeData.Month] >= BeginDay && DayAndMonthInt[(int)TypeData.Month] <= EndDay;
        bool IsHoliday = OnlyDay || !OnlyDay && MultyDays;
        if(IsHoliday)
        {
            Objects.ForEach(h => h.Action());
            Action.Invoke();
        }
        return IsHoliday;
    }
}
