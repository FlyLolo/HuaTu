﻿namespace DrawWork.Command
{
    public  interface ICommand
    {
        void Execute();
        void UnExecute();
    }
}