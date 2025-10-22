# Codeconventions for this project

## Naming Conventions
Variables: lowerCamelCase  
Methods: UpperCamelCase()  
Classes: UpperCamelCase  
Inspector: UpperCamelCase  

## How to use {}
In C# the convention is putting the opening Bracket on a new line, we will use this convention as well. Example:
```
if(something)
{
    // my cool code
}

private void MyMethod()
{
    // more cool code
}
```

## Privacy rules
Keep every variable and method private by default. Only expose if needed.
This also applies to variables you wish to expose to the editor, use ``[SerializeField]`` instead of making everything public.
