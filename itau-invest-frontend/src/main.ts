import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { App } from './app/app';


bootstrapApplication(App, {
  providers: [
    provideHttpClient(), 
    provideAnimations()  
  ]
}).catch(err => console.error(err));