﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IActivatable
{
    /// <summary>
    /// Обязательное свойство для отслеживания типа меню, в котором находится игрок
    /// </summary>
    public TypeMenu TypeMenu { get; }
    /// <summary>
    /// Для разовой активации статических полей в классах из пространства имен GroupMenu
    /// </summary>
    void Start();
    /// <summary>
    /// Происходит при Активации меню
    /// </summary>
    void Activate();
    /// <summary>
    /// Происходит при Диактивации меню
    /// </summary>
    void Deactivate();
}