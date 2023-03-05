# Fluid Template Engine Plugin

Fluid is an open-source .NET template engine based on the Liquid template language. It's a secure template language that is also very accessible for non-programmer audiences.

## Functions

| Name and Description |
| --- |
| `fluid_template(template: string, out: string, var_name: string = 'rows'): object<IRowsOutput>`<br /><br /> Writes data to a Fluid template. |

- `template`. Path to the template file.
- `out`. Path to the output file.
- `var_name`. Variable name used in template with the result data. Default is `rows`.

## Examples

***Render HTML template from Github data***

```html
<!doctype html>
  <body>
    <!-- The special "run" block allows embed SQL. -->
    {% run %}
      select gc.sha, sum((select additions from github_commits_ref('krasninja/querycat', gc.sha))) as 'adds'
      from github_commits('krasninja/querycat') as gc
      where author_date >= '2022-11-01' and author_date <= '2022-11-03'
    {% endrun %}
    <ul>
      <!-- Iterate thru result and render. -->
      {% for row in rows %}
        <li> {{ row["adds"] }} </li>
      {% endfor %}
    <ul>
  </body>
</html>
```

Query: `fluid_template('/home/ivan/temp/demo.liquid', '/home/ivan/demo/demo.html')`

## Links

- https://github.com/sebastienros/fluid
