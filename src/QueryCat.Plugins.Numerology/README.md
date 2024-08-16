# QueryCat Plugin for Numerology

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

Calculates Pythagoras matrix that is widely used in numerology. The output format is the following: `11,1111 222 5 7 88 99`. The first number is the "fate number". After the comma the matrix numbers are following.

## Example Queries

```
numerology_calc_matrix('1985-11-14'::timestamp);
```
