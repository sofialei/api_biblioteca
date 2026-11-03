// Resuelve la ambigüedad entre GreenDonut.KeyNotFoundException (HotChocolate)
// y System.Collections.Generic.KeyNotFoundException en todo el proyecto.
global using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;
