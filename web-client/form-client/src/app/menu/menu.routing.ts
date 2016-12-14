import { NgModule, OnInit } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Logger } from 'libclient';
import { ModuleWithProviders } from '@angular/core';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
const menuRoutes: Routes = [
    {
        path: 'menu',
        children: [
            { path: 'favorites', component: FavoritesComponent, outlet: 'favorites' }
        ],
    },
];
export const menuRouting: ModuleWithProviders = RouterModule.forChild(menuRoutes);