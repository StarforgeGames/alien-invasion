﻿namespace LispInterpreter
{
    struct LispQuote : LispElement
    {
        public dynamic Eval(LispEnvironment env, dynamic e = null)
        {
            if (e == null)
            {
                return this;
            }
            else
            {
                return e.First;
            }
        }

        public override string ToString()
        {
            return "'";
        }
    }
}
