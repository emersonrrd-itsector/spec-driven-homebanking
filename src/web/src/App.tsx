import { Button } from './components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './components/ui/card'

function App() {
  return (
    <div className="min-h-screen bg-background text-foreground">
      <main className="container mx-auto p-8">
        <div className="space-y-8">
          <section>
            <h1 className="text-4xl font-bold mb-2">Welcome to Home Banking</h1>
            <p className="text-muted-foreground">Built with React 19, Vite, TypeScript, and Tailwind CSS</p>
          </section>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            <Card>
              <CardHeader>
                <CardTitle>Dark Theme</CardTitle>
                <CardDescription>Enabled by default</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">This application uses shadcn/ui components with a dark theme as default.</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>React 19</CardTitle>
                <CardDescription>Latest version</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">Built with the latest React framework for optimal performance.</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>TypeScript 5.7</CardTitle>
                <CardDescription>Strict mode enabled</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm">Full type safety with strict TypeScript compilation.</p>
              </CardContent>
            </Card>
          </div>

          <div className="flex gap-4">
            <Button variant="default">Primary Button</Button>
            <Button variant="outline">Outline Button</Button>
            <Button variant="secondary">Secondary Button</Button>
          </div>
        </div>
      </main>
    </div>
  )
}

export default App
