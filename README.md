The code is pretty well documented
===

But I will put some formulas in here to complete this.

In the meantime;
---
```
    2D Regression Generator (Bi-variate)
    Lineage from 1978 TI-58 and TI-59 calculators.  Ported to C in 1982, ported to Perl 3 in 1987.
    Parameter names are more 'traditional' than newer code and am not going to upgrade them at this time.
    Copyright (c) Jillian England, 2001, 2002, 2003, 2008, 2012, 2016, 2018, 2019, 2022

    I am a 2D Regression generator
    I can add pairs of x, y data to myself
    I can return the following properties of that data
    Two dimensional statistical data
    sy  = sum y
    sy2 = sum y**2
    n
    sx  = sum x
    sx2 = sum x**2
    sxy = sum x * y
    mean x = mx = sx/n
    mean y = my = sy/n
    standard deviation x = qx = sqr((sx2 - (sx**2 /n)) / (n-1) )
    standard deviation y = qy = sqr((sy2 - (sy**2 /n)) / (n-1) )
    variance x = qx2 = sx2 / n - mx**2
    variance y = qy2 = sy2 / n - my**2
    Use N weighting for population studies
    and N-1 for sample studies
    Slope = m = sxy - (sx*sy)/n / sx2 - sx**2 /n
    yIntercept = b = (sy - m*sx) / n
    correlation coefficient = R = (m qx) /qy
```
